using FluentMigrator;

namespace ClipboardMachinery.Core.DataStorage.Migrations {

    [Migration(1)]
    public class AddPresenterColumn : Migration {

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
