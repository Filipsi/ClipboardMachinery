using FluentMigrator;

namespace ClipboardMachinery.Core.DataStorage.Migrations {

    [Migration(1, "Add 'Presenter' column to the 'Clip' table. This allows user to specify display format.")]
    public class AddClipPresenterColumn : Migration {

        public override void Up() {
            if (!Schema.Table("Clip").Column("Presenter").Exists()) {
                Create.Column("Presenter").OnTable("Clip").AsString().Nullable();
            }
        }

        public override void Down() {
            Delete.Column("Presenter").FromTable("Clip");
        }

    }

}
