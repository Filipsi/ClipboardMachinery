using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ClipboardMachinery.Core.DataStorage {

    public interface IDataRepository : IDisposable {

        /// <summary>
        /// Cached content of last clip saved in the database.
        /// This property is loaded at initialization and automatically updated when new clip is saved.
        /// </summary>
        string LastClipContent { get; }

        /// <summary>
        /// Create clip provider that can be used to restive batches of clips sored by descending date.
        /// </summary>
        /// <param name="batchSize">Size of a batch of clips</param>
        /// <returns>A instance of lazy clip provider</returns>
        ILazyDataProvider CreateLazyClipProvider(int batchSize);

        /// <summary>
        /// Create new clip with specified values and insert it into repository.
        /// </summary>
        /// <typeparam name="T">A type of data model that created clip instance will be mapped to and returned back</typeparam>
        /// <param name="content">Content of a clip</param>
        /// <param name="created">Date and time when clip was created</param>
        /// <param name="tags">Tags that clip have in format name=value</param>
        /// <returns>An instance of created clip mapped to T model</returns>
        Task<T> CreateClip<T>(string content, DateTime created, KeyValuePair<string, object>[] tags = null);

        /// <summary>
        /// Remove clip with corresponding id and all related tags.
        /// </summary>
        /// <param name="id">Id of a clip to remove</param>
        Task DeleteClip(int id);

        /// <summary>
        /// Create new tag for a clip.
        /// When TagType specified by the name does not exist, it will be created with supplied value type used as a data type.
        /// </summary>
        /// <typeparam name="T">A type of data model that created tag instance will be mapped to and returned back</typeparam>
        /// <param name="clipId">Id of a clip that this tag is related to</param>
        /// <param name="name">Name of the tag that should be created. This corresponds to tag type definition.</param>
        /// <param name="value">Value of the tag that will be created. If TagType specified by the name does not exist, this value data type will be used as newly created TagType's data type.</param>
        /// <returns>An instance of created tag mapped to T model</returns>
        Task<T> CreateTag<T>(int clipId, string name, object value);

        /// <summary>
        /// Update value of tag with corresponding id.
        /// </summary>
        /// <param name="id">Id of tag to update</param>
        /// <param name="value">A new value for the tag</param>
        Task UpdateTag(int id, object value);

        /// <summary>
        /// Remove tag with corresponding id.
        /// Used to remove tag from a clip.
        /// </summary>
        /// <param name="id">Id of tag to remove</param>
        Task DeleteTag(int id);

        /// <summary>
        /// Create tag type provider that can be used to restive batches of tag types sored by descending creation date.
        /// </summary>
        /// <param name="batchSize">Size of a batch of tag types</param>
        /// <returns>A instance of lazy tag type provider</returns>
        ILazyDataProvider CreateLazyTagTypeProvider(int batchSize);

        /// <summary>
        /// Updates TagType with corresponding name (primary key).
        /// TagType name is equivalent to TagModel#Name.
        /// </summary>
        /// <param name="typeName">Id of TagType aka TagModel#Name</param>
        /// <param name="color">New color of TagType</param>
        Task UpdateTagType(string typeName, Color color);

    }

}
