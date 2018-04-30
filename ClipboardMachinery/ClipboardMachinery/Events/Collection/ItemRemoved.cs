namespace ClipboardMachinery.Events.Collection {

    internal class ItemRemoved<T> : ItemBase<T> {

        public ItemRemoved(T item) : base(item) {
        }

    }

}
