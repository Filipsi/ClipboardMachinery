﻿using System;
using ClipboardMachinery.Components.Clip;
using ClipboardMachinery.Components.ContentPresenter;
using ClipboardMachinery.Components.ContentPresenter.Impl.BatchScriptPresenter;
using ClipboardMachinery.Components.ContentPresenter.Impl.ImagePresenter;
using ClipboardMachinery.Components.ContentPresenter.Impl.LinkPresenter;
using ClipboardMachinery.Components.ContentPresenter.Impl.TextPresenter;

namespace ClipboardMachinery.Plumbing.Factories {

    public interface IContentScreenFactory {

        TextContentScreenViewModel CreateTextScreen(IContentPresenter creator, ClipViewModel owner, Action<ContentScreen> releaseFn);

        LinkContentScreenViewModel CreateLinkScreen(IContentPresenter creator, ClipViewModel owner, Action<ContentScreen> releaseFn);

        ImageContentScreenViewModel CreateImageScreen(IContentPresenter creator, ClipViewModel owner, Action<ContentScreen> releaseFn);

        BatchScriptContentScreenViewModel CreateBatchScriptScreen(IContentPresenter creator, ClipViewModel owner, Action<ContentScreen> releaseFn);

        void Release(ContentScreen contentScreen);

    }

}
