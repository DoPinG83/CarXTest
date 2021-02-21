using System;
using System.Collections.Generic;
using System.Linq;
using Core.Gui.Carousel;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Gui
{
    /// <typeparam name="TData"></typeparam>
    internal class ChatCarousel<TData> : Carousel<TData>
    {
        public ChatCarousel(CarouselScroller scroller, ICarouselFactory<TData> itemFactory,
            Action<ICarouselItem<TData>> environmentInitializer = null, Vector2? overrideSwapSize = null,  int extraItemsCount = 2) : base(
            scroller, itemFactory, environmentInitializer, overrideSwapSize, extraItemsCount) 
        {
        }

        int _extraItemWidth = 400; // 1 request
        public void InsertNewMessage(TData itemData)
        {
            bool filled = CalcSize() - _extraItemWidth >= _containerSize;
            _data.Insert(0, itemData);
            
            if (!filled){
                InitItems(0, null, true);
            } else{
                TData lastData = _items.Last.Value.Data;
                var idx = _data.IndexOf(lastData);
                if (idx <= _items.Count){
                    var i = _data.IndexOf(_items.First.Value.Data);
                    foreach (var itm in _items){
                        itm.Init(_data[i - 1]);
                        i++;
                    }
                }
            }
        }

        public void UpdateData(TData oldData, TData newData)
        {
            var dataUpd = _data.FirstOrDefault(x => x.Equals(oldData));
            if (dataUpd != null){
                var idx = _data.IndexOf(dataUpd);
                _data[idx] = newData;

                var updItem = _items.FirstOrDefault(i => i.Data.Equals(oldData));
                if (updItem != null){
                    updItem.Init(_data[idx]);
                }
            }
        }

        public void UpdateDataHistory(List<TData> newData)
        {
            _data = newData;
        }

        public TData GetFirstVisibleElement()
        {
            TData lastData = _items.First.Value.Data;
            return lastData;
        }

        public void SkipItem()
        {
            SwapForward();
        }

        protected override Vector2? SwapBackward()
        {
            var idx = _data.IndexOf(_items.First.Value.Data);
            if (!_looped && idx <= 0){
                if (swapUpdate != null)
                    swapUpdate();
                return null;
            }

            Vector2 size = Vector2.zero;
            Vector2 sizePrev = Vector2.zero;
            Vector2 sizeNew = Vector2.zero;
            var tmp = new Queue<ICarouselItem<TData>>();
            for (int i = 0; i < _rowMultiplier; ++i){
                ICarouselItem<TData> item;
                if (_extraItems.Count > 0){
                    item = _extraItems.Pop();
                }
                else{
                    item = _items.Last.Value;
                    _items.RemoveLast();
                    item.Clear();
                }

                tmp.Enqueue(item);
                if (i == 0){
                    size = _overrideSwapSize ??
                           item.Rect.rect
                               .size; //new Vector2(item.Rect.rect.size.x, item.Rect.GetComponent<LayoutElement>().minHeight);//item.Rect.rect.size;
                }

                sizePrev = new Vector2(item.Rect.rect.size.x,
                    item.Rect.GetComponent<LayoutElement>().minHeight); //new Vector2(size.x, size.y);
            }

            for (int i = 0; i < _rowMultiplier; ++i){
                var item = tmp.Dequeue();
                if (_reverseItems){
                    item.Rect.SetAsLastSibling();
                }
                else{
                    item.Rect.SetAsFirstSibling();
                }

                if (_looped || idx > 0){
                    item.Rect.gameObject.SetActive(true);
                    _items.AddFirst(item);
                    var d = _data[GetIndex(idx - 1)];
                    item.Init(d);
                    sizeNew = new Vector2(item.Rect.rect.size.x, item.Rect.GetComponent<LayoutElement>().minHeight);
                }

                idx--;
            }

            if (swapUpdate != null)
                swapUpdate();

            _scroller.Content.anchoredPosition = new Vector2(_scroller.Content.anchoredPosition.x,
                _scroller.Content.anchoredPosition.y + (sizePrev.y - sizeNew.y));
            return size + Vector2.one * _spacing;
        }
    }
}
