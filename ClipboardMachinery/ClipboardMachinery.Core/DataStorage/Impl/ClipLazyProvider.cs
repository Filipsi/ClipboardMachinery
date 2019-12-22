using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ClipboardMachinery.Core.DataStorage.Schema;
using ServiceStack.OrmLite;

namespace ClipboardMachinery.Core.DataStorage.Impl {

    public class ClipLazyProvider : GenericLazyProvider<Clip> {

        #region Fields

        private string filteredTagName;
        private string filteredTagValue;

        #endregion

        internal ClipLazyProvider(DataRepository dataRepository, int batchSize) : base(dataRepository, batchSize) {
        }

        #region Logic

        /// <summary>
        /// Specifies a filter that will be applied to each batch query.
        /// NOTE: This is only experimental, more in-depth implementation will be needed once we start working on search.
        /// </summary>
        /// <param name="name">Name of a tag to filter or null if you do no with to filter by this property.</param>
        /// <param name="value">Value of a tag to filter or null if you do no with to filter by this property.</param>
        public void ApplyTagFilter(string name, string value) {
            filteredTagName = name;
            filteredTagValue = value;
        }

        #endregion

        #region Lifecycle

        protected override Task OnQueryBuildingStarts(SqlExpression<Clip> query) {
            // NOTE: More in-depth implementation will be needed once we start working on search.
            if (!string.IsNullOrEmpty(filteredTagName) && !string.IsNullOrEmpty(filteredTagValue)) {
                query
                    .LeftJoin<Tag>()
                    .Join<Tag, TagType>()
                    .Where<Tag>(tag => tag.Type.Name == filteredTagName && tag.Value.ToString() == filteredTagValue);
            }

            return base.OnQueryBuildingStarts(query);;
        }

        protected override Task OnQueryOrdering(SqlExpression<Clip> query) {
            // Order clips by ID column
            query.OrderByDescending(clip => clip.Id);
            return Task.CompletedTask;
        }

        protected override async Task OnBatchLoaded(IDbConnection db, List<Clip> batch) {
            await base.OnBatchLoaded(db, batch);

            // Go thought every single clip in the batch
            foreach (Clip clip in batch.Where(clip => clip.Tags != null)) {
                // Load nested references for clip tags
                foreach (Tag tag in clip.Tags) {
                    await db.LoadReferencesAsync(tag);
                }
            }
        }

        #endregion

    }

}
