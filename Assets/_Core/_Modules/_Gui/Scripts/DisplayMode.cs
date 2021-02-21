using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Gui
{
    /// <summary>
    /// Режим отображения ГУИ
    /// </summary>
    public enum DisplayMode
    {
        /// <summary>
        /// Постоянный, закрывается только при соответсвующем вызове Close
        /// </summary>
        Persistent,

        /// <summary>
        /// Режим скрина, закрывается автоматом при показе другого скрина через Show
        /// </summary>
        Screen,

        /// <summary>
        /// Модальный диалог, добавляется поверх открытых скринов/диалогов
        /// </summary>
        DialogAddFront,

        /// <summary>
        /// Модальный диалог, закрывает текущие отображаемые диалоги
        /// </summary>
        DialogCloseOthers,

        /// <summary>
        /// Модальный диалог, скрывает текущий скрин
        /// </summary>
        DialogHideScreen,

        /// <summary>
        /// Модальный диалог, не производит никаких действий с другими скринами и диалогами
        /// </summary>
        Dialog,
    }
}
