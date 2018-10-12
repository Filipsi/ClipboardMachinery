namespace ClipboardMachinery.Events {

    public class ItemRemoved<T> : ItemHolder<T> {

        public ItemRemoved(T item) : base(item) {
        }

    }

}
