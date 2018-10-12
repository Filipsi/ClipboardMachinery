namespace ClipboardMachinery.Events.Collection {

    public abstract class ItemHolder<T> {

        public T Item { get; }

        protected ItemHolder(T item) {
            Item = item;
        }

    }

}
