namespace ClipboardMachinery.Events.Collection {

    internal abstract class ItemBase<T> {

        public T Item { get; }

        protected ItemBase(T item) {
            Item = item;
        }

    }

}
