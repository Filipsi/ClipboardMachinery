﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using ClipboardMachinery.Core.DataStorage.Schema;
using ServiceStack.OrmLite;

namespace ClipboardMachinery.Core.DataStorage.Impl {

    public class LazyDataProvider<T> : ILazyDataProvider {

        #region Properties

        public int BatchSize { get; }

        public int Offset { get; set; }

        #endregion

        #region Fields

        private readonly DataRepository dataRepository;
        private readonly Func<IDbConnection, IList<T>, Task> onBatchLoaded;

        private string filteredTagName;
        private string filteredTagValue;

        #endregion

        internal LazyDataProvider(DataRepository dataRepository, int batchSize, Func<IDbConnection, IList<T>, Task> onBatchLoaded = null) {
            this.dataRepository = dataRepository;
            this.onBatchLoaded = onBatchLoaded;
            BatchSize = batchSize;
        }

        #region Logic

        public async Task<IEnumerable<TM>> GetNextBatchAsync<TM>() {
            IDbConnection db = dataRepository.Database.Connection;

            // Create SQL query
            SqlExpression<T> query = db.From<T>();

            // Apply filter
            // NOTE: This is only experimental, more in-depth implementation will be needed once we start working on search.
            //       Currently this is used to filter favorite clips.
            if (typeof(T).IsAssignableFrom(typeof(Clip))) {
                if (!string.IsNullOrEmpty(filteredTagName) && !string.IsNullOrEmpty(filteredTagValue)) {
                    query
                        .LeftJoin<Tag>()
                        .Join<Tag, TagType>()
                        .Where<Tag>(tag => tag.Type.Name == filteredTagName && tag.Value.ToString() == filteredTagValue);
                }
            }

            query.OrderByDescending(1).Limit(BatchSize);
            query.Offset = Offset;

            // Load entries of type T
            List<T> entries = await db.LoadSelectAsync(query);

            // Run batch creation hooks
            // This is used to perform type specific action when batch is loaded from the database
            // For example to load nested references
            if (onBatchLoaded != null) {
                await onBatchLoaded.Invoke(db, entries);
            }

            // Move offset of lazy loader
            Offset += entries.Count;

            // Map T results to desired models
            return dataRepository.Mapper.Map<TM[]>(entries);
        }

        public void ApplyTagFilter(string name, string value) {
            filteredTagName = name;
            filteredTagValue = value;
        }

        #endregion

    }

}