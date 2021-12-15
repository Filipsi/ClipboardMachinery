using FluentMigrator;

namespace ClipboardMachinery.Core.DataStorage.Migrations {

    [Migration(2, "Add 'Priority' column to the 'TagType' table. This allows user to specify the display order of tags.")]
    public class AddTagTypePriority : Migration {

        public override void Up() {
            if (!Schema.Table("TagType").Column("Priority").Exists()) {
                Create.Column("Priority").OnTable("TagType").AsByte().WithDefaultValue(0);
            }
        }

        public override void Down() {
            Delete.Column("Priority").FromTable("TagType");
        }

    }

}
