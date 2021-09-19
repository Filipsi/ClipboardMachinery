using FluentMigrator;

namespace ClipboardMachinery.Core.DataStorage.Migrations {

    [Migration(0, "Remove 'Created' column from the 'Clip' table. This is now handled by tag system.")]
    public class RemoveClipCreatedColumn : Migration {

        public override void Up() {
            if (Schema.Table("Clip").Column("Created").Exists()) {
                Delete.Column("Created").FromTable("Clip");
            }
        }

        public override void Down() {
            Create.Column("Created").OnTable("Clip");
        }

    }

}
