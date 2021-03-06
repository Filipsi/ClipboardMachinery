﻿using Caliburn.Micro;
using ClipboardMachinery.Components.Navigator;

namespace ClipboardMachinery.Pages.Search {

    public class SearchViewModel : Screen, IScreenPage {

        #region IScreenPage

        public string Title
            => "Search";

        public string Icon
            => "IconSearch";

        public byte Order
            => 3;

        #endregion

    }

}
