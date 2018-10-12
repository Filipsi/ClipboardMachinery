namespace ClipboardMachinery.Events.Collection {

    public class ItemRemoved<T> : ItemHolder<T> {

        public ItemRemoved(T item) : base(item) {
        }

    }

}
