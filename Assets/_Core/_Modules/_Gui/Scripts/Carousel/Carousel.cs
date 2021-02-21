using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Gui.Carousel;
using UnityEngine;
using UnityEngine.UI;
using Core.Utils;

namespace Core.Gui
{
    /// <typeparam name="TData"></typeparam>
    internal class Carousel<TData>
    {
        protected readonly CarouselScroller _scroller;
        private readonly ICarouselFactory<TData> _itemFactory;
        private readonly Action<ICarouselItem<TData>> _environmentInitializer;

        protected IList<TData> _data;
        protected readonly LinkedList<ICarouselItem<TData>> _items = new LinkedList<ICarouselItem<TData>>();
        protected readonly Stack<ICarouselItem<TData>> _extraItems = new Stack<ICarouselItem<TData>>();

        protected readonly float _containerSize;
        protected readonly float _spacing;
        protected readonly int _rowMultiplier = 1;
        protected readonly bool _isHorizontal;
        protected bool _reverseItems = false;

        protected Vector2? _overrideSwapSize;

        public Action swapUpdate;
        public Action swapEnd;

        protected int _extraItemsCount;

        protected bool _looped { get { return _scroller.Looped; } }

        public Carousel(CarouselScroller scroller, ICarouselFactory<TData> itemFactory, Action<ICarouselItem<TData>> environmentInitializer = null, Vector2? overrideSwapSize = null, int extraItemsCount = 2)
        {
            _scroller = scroller;
            _itemFactory = itemFactory;
            _environmentInitializer = environmentInitializer;
            _extraItemsCount = extraItemsCount;

            _isHorizontal = _scroller is CarouselHorizontalScroller;
            _containerSize = GetSize(_scroller.ViewRect.rect, _isHorizontal);

            _overrideSwapSize = overrideSwapSize;

            var layout = _scroller.Content.GetComponent<HorizontalOrVerticalLayoutGroup>();
            if (layout != null)
                _spacing = layout.spacing;
            else
            {
                var grid = _scroller.Content.GetComponent<GridLayoutGroup>();
                _spacing = _isHorizontal ? grid.spacing.x : grid.spacing.y;
                switch (grid.constraint)
                {
                    case GridLayoutGroup.Constraint.FixedRowCount:
                    case GridLayoutGroup.Constraint.FixedColumnCount:
                        _rowMultiplier = grid.constraintCount;
                        break;
                    case GridLayoutGroup.Constraint.Flexible:
                        float opScrollSize = GetSize(_scroller.ViewRect.rect, !_isHorizontal);
                        var opCellSize = _isHorizontal ? grid.cellSize.y : grid.cellSize.x;
                        var opSpacing = _isHorizontal ? grid.spacing.y : grid.spacing.x;
                        int maxCount = 1;
                        float totalSize = opCellSize;
                        while (true)
                        {
                            totalSize += opSpacing + opCellSize;
                            if (totalSize >= opScrollSize)
                                break;
                            ++maxCount;
                        }
                        _rowMultiplier = maxCount;
                        break;
                }
            }
        }

        public void ResetScroller()
        {
            _scroller.StopMovement();
            _scroller.Content.anchoredPosition = Vector2.zero;
        }

        public void Init(IList<TData> data, bool isReversed = false, Action cb = null, int? idx = null)
        {
            _reverseItems = isReversed;
            if (_reverseItems){
                _scroller.SetSwapHandlers(SwapBackward, SwapForward);
            } else{
                _scroller.SetSwapHandlers(SwapForward, SwapBackward);
            }
            if (data == null)
            {
                CoreLog.LogError("Null argument: data");
                return;
            }
            _data = data;
            if (data.Count == 0)
                return; // не было передано ни одного айтема

            InitItems(idx, cb);
        }

        private bool ScrollVolumeFilled()
        {
            bool filled = CalcSize() >= _containerSize;
            if (filled && _rowMultiplier > 1)
                return _items.Count % _rowMultiplier == 0;
            return filled;
        }

        public int? GetFirstIndex()
        {
            if (_data == null || _items == null || _items.Count == 0)
                return null;
            var idx = _data.IndexOf(_items.First.Value.Data);
            if (idx == -1)
                return null;
            return idx;
        }

        protected void InitItems(int? initialIndx, Action cb, bool specialInit = false)
        {
            //CoreLog.Log("InitItems w/ " + _data.Count);
            var placeFirst = false;
            int idx = initialIndx.GetValueOrDefault();
            int extraItems = 0;
            bool reverse = true;
            while (true){
                bool isExtraItem = false;
                // заполняем элементами, пока не заполним предоставленный скроллером объем
                bool volumeFilled = ScrollVolumeFilled();
                if ((volumeFilled && (!reverse || !_looped)) || extraItems > 0)
                    // впринципе эти 2 дополнительных айтема не всегда нужны, 
                    // TODO: рассчитать, нужна ли пара доп. айтемов
                if (extraItems++ >= _extraItemsCount * _rowMultiplier)
                        break;

                // данные, которыми инициализируем элемент
                if (!_looped && idx >= _data.Count){
                    if (_items.Count < idx && extraItems == 0){
                        idx = initialIndx.GetValueOrDefault() - 1;
                        placeFirst = true;
                    } else if (extraItems > 0)
                        isExtraItem = true;
                    else
                        break;
                }

                // создем айтемы
                var item = _itemFactory.Create(_scroller.Content);
                if (_environmentInitializer != null)
                    _environmentInitializer(item);

                if (isExtraItem){
                    _extraItems.Push(item);
                    item.Rect.gameObject.SetActive(false);
                    continue;
                }

                var i = GetIndex((reverse && _looped) ? -idx : idx);
                var d = _data[i];
                item.Init(d);

                if (reverse && _looped || placeFirst || specialInit){
                    if (_reverseItems){
                        item.Rect.SetAsLastSibling();
                    } else{
                        item.Rect.SetAsFirstSibling();
                    }
                    _items.AddFirst(item);
                } else{
                    if (_reverseItems){
                        item.Rect.SetAsFirstSibling();
                    } else{
                        item.Rect.SetAsLastSibling();
                    }
                    _items.AddLast(item);
                }

                if (_items.Count >= _data.Count)
                    break;

                // индекс для инициализации след. элемента
                if (placeFirst)
                    idx--;
                else if (!_looped || reverse)
                    ++idx;
                reverse = !reverse;
            }

            if (initialIndx != null && !specialInit){
                var scrollToItem = _items.First(i => i.Data.Equals(_data[initialIndx.Value]));
                _scroller.ScrollTo(scrollToItem.Rect, cb);
            }
        }

        public void ReplaceData(TData oldData, TData newData)
        {
            var index = _data.IndexOf(oldData);
            if (index != -1) {
                _data[index] = newData;
            }

            var targetItem = _items.FirstOrDefault(z => z.Data.Equals(oldData));
            if (targetItem != null) {
                targetItem.Init(newData);
            }
        }

        public RectTransform ScrollTo(TData data, Action cb = null)
        {
            var targetItem = _items.FirstOrDefault(z => z.Data.Equals(data));
            if (targetItem != null)
            {
                _scroller.ScrollTo(targetItem.Rect, cb);
                return targetItem.Rect;
            }
            int idx = _data.IndexOf(data);
            if (idx >= 0)
            {
                ClearItems();
                InitItems(idx, cb);
                var scrollToItem = _items.First(i => i.Data.Equals(_data[idx]));
                return scrollToItem.Rect;
            }
            //Tracer.LogWarning(string.Format("No object with data {0} to scroll to", data));
            return null;
        }

        public void Clear()
        {
            _scroller.SetSwapHandlers(null, null);
            ClearItems();
        }

        public void UpdateCurrentItems()
        {
            foreach (var item in _items) {
                item.Clear();
                item.Init(item.Data);
            }
        }

        public void UpdateItem(TData itemData)
        {
            var item = _items.FirstOrDefault(z => z.Data.Equals(itemData));
            if (item != null) {
                item.Clear();
                item.Init(itemData);
            }
        }

        private void ClearItems()
        {
            _scroller.Clear();
            foreach (var item in _items)
                item.Dispose();
            _items.Clear();
            _extraItems.Clear();
        }

        protected int GetIndex(int idx)
        {
            if (idx == 0)
                return 0;
            int count = _data.Count;
            var div = idx % count;
            if (idx > 0)
                return div;
            return (count + div) % count;
        }

        private float GetSize(Rect rect, bool horizontal)
        {
            return horizontal
                ? rect.width
                : rect.height;
        }

        protected float CalcSize()
        {
            float size = 0;
            int i = 0;
            foreach (var item in _items)
            {
                if (i++ % _rowMultiplier != 0)
                    continue;
                size += GetSize(item.Rect.rect, _isHorizontal);
            }
            int entryCount = _items.Count / _rowMultiplier + (_items.Count % _rowMultiplier == 0 ? 0 : 1);
            size += entryCount > 1
                ? (entryCount - 1) * _spacing
                : 0;
            return size;
        }

        #region Swap handling

        protected Vector2? SwapForward()
        {            
            TData lastData = _items.Last.Value.Data;
            var idx = _data.IndexOf(lastData);
            if (!_looped && idx >= _data.Count - 1)
            {
                if (swapUpdate != null)
                    swapUpdate();
                if (swapEnd != null){
                    swapEnd();
                }

                return null;
            }

            Vector2 size = Vector2.zero;
            var tmp = new Queue<ICarouselItem<TData>>();
            for (int i = 0; i < _rowMultiplier; ++i)
            {
                var item = _items.First.Value;
                tmp.Enqueue(item);
                if (i == 0)
                    size = _overrideSwapSize ?? item.Rect.rect.size;
                _items.RemoveFirst();
                item.Clear();
            }

            for (int i = 0; i < _rowMultiplier; ++i)
            {
                var item = tmp.Dequeue();
                if (_reverseItems){
                    item.Rect.SetAsFirstSibling();
                } else{
                    item.Rect.SetAsLastSibling();
                }
                if (_looped || idx < _data.Count - 1)
                {
                    _items.AddLast(item);
                    var d = _data[GetIndex(idx + 1)];
                    item.Init(d);
                }
                else
                {
                    _extraItems.Push(item);
                    item.Rect.gameObject.SetActive(false);
                }
                idx++;
            }

            if (swapUpdate != null)
                swapUpdate();

            return size + Vector2.one * _spacing;
        }

        protected virtual Vector2? SwapBackward()
        {
            if (_items.Count == 0) {
                return null;
            }

            var idx = _data.IndexOf(_items.First.Value.Data);
            if (!_looped && idx <= 0)
            {
                if (swapUpdate != null)
                    swapUpdate();

                return null;
            }

            Vector2 size = Vector2.zero;
            var tmp = new Queue<ICarouselItem<TData>>();
            for (int i = 0; i < _rowMultiplier; ++i)
            {
                ICarouselItem<TData> item;
                if (_extraItems.Count > 0){
                    item = _extraItems.Pop();
                } else{
                    item = _items.Last.Value;
                    _items.RemoveLast();
                    item.Clear();
                }
                tmp.Enqueue(item);
                if (i == 0){
                    size = _overrideSwapSize ?? item.Rect.rect.size;
                }
            }

            for (int i = 0; i < _rowMultiplier; ++i)
            {
                var item = tmp.Dequeue();
                if (_reverseItems){
                    item.Rect.SetAsLastSibling();
                } else{
                    item.Rect.SetAsFirstSibling();
                }
                if (_looped || idx > 0)
                {
                    item.Rect.gameObject.SetActive(true);
                    _items.AddFirst(item);
                    var d = _data[GetIndex(idx - 1)];
                    item.Init(d);
                    if (i == 0) {
                        size = _overrideSwapSize ?? item.Rect.rect.size;
                    }
                }
                idx--;
            }

            if (swapUpdate != null)
                swapUpdate();

            return size + Vector2.one * _spacing;
        }

        #endregion
    }
}
