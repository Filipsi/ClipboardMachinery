using System;
using ClipboardMachinery.Windows.UpdateNotes;

namespace ClipboardMachinery.Plumbing.Factories {

    public interface IWindowFactory {

        UpdateNotesViewModel CreateUpdateNotesWindow(Version tagVersion);

        void Release(UpdateNotesViewModel updateNotes);

    }

}
