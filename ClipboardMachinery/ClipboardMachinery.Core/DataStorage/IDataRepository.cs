using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClipboardMachinery.Core.DataStorage.Impl;
using ClipboardMachinery.Core.DataStorage.Schema;
using ClipboardMachinery.Core.TagKind;
using System.Windows.Media;

namespace ClipboardMachinery.Core.DataStorage {

    public interface IDataRepository : IDisposable {

        /// <summary>
        /// Cached content of last clip saved in the database.
        /// This property is loaded at initialization and automatically updated when new clip is saved.
        /// </summary>
        string LastClipContent { get; }

        /// <summary>
        /// Create clip provider that can be used to restive batches of clips sorted by descending date.
        /// </summary>
        /// <param name="batchSize">Size of a batch of clips</param>
        /// <returns>A instance of lazy clip provider</returns>
        ClipLazyProvider CreateLazyClipProvider(int batchSize);

        /// <summary>
        /// Create new clip with specified values and insert it into repository.
        /// </summary>
        /// <param name="content">Content of a clip</param>
        /// <param name="contentPresenter">Id of an presenter used to display the content</param>
        /// <param name="tags">Tags that clip have in format name=value</param>
        /// <returns>The created clip entity</returns>
        Task<ClipEntity> CreateClip(string content, string contentPresenter, KeyValuePair<string, object>[] tags = null);

        /// <summary>
        /// Update content presenter of the specified clip id.
        /// </summary>
        /// <param name="id">Id of a clip to update</param>
        /// <param name="contentPresenter">Id of an presenter used to display the content</param>
        Task UpdateClip(int id, string contentPresenter);

        /// <summary>
        /// Remove clip with corresponding id and all related tags.
        /// </summary>
        /// <param name="id">Id of a clip to remove</param>
        /// <returns>A task that will be completed after the operation is performed.</returns>
        Task DeleteClip(int id);

        /// <summary>
        /// Create new tag for a clip.
        /// When TagType specified by the name does not exist, it will be created with supplied value type used as a data type.
        /// </summary>
        /// <param name="clipId">Id of a clip that this tag is related to</param>
        /// <param name="tagType">Name of the tag that should be created. This corresponds to tag type definition.</param>
        /// <param name="value">Value of the tag that will be created. If TagType specified by the name does not exist, this value data type will be used as newly created TagType's data type.</param>
        /// <returns>The created tag entity, or null when the value could not be persisted</returns>
        Task<TagEntity> CreateTag(int clipId, string tagType, object value);

        /// <summary>
        /// Attempts to find a Tag based on it's id property.
        /// </summary>
        /// <param name="tagId">Id of a tag that should be found.</param>
        /// <returns>The matching tag entity, or null when there is none</returns>
        Task<TagEntity> FindTag(int tagId);

        /// <summary>
        /// Update value of tag with corresponding id.
        /// </summary>
        /// <param name="id">Id of tag to update</param>
        /// <param name="value">A new value for the tag</param>
        /// <returns>A task that will be completed after the operation is performed with new persisted value.</returns>
        Task<string> UpdateTag(int id, object value);

        /// <summary>
        /// Remove tag with corresponding id.
        /// Used to remove tag from a clip.
        /// </summary>
        /// <param name="id">Id of tag to remove</param>
        Task DeleteTag(int id);

        /// <summary>
        /// Create tag type provider that can be used to restive batches of tag types sorted by descending creation date.
        /// </summary>
        /// <param name="batchSize">Size of a batch of tag types</param>
        /// <returns>A instance of lazy tag type provider</returns>
        ILazyDataProvider<TagTypeEntity> CreateLazyTagTypeProvider(int batchSize);

        /// <summary>
        /// Create a new tag type with given properties.
        /// </summary>
        /// <param name="name">A name of newly created tag type.</param>
        /// <param name="description">Description for newly created tag type.</param>
        /// <param name="kind">A type of values that can be accepted by this tag type, the actual parsing logic is handled by corresponding <see cref="ITagKindSchema"/> implementation.</param>
        /// <param name="priority">Display priority of the tag type</param>
        /// <param name="color">A color of newly created tag type, if color is not specified a default color will be used <see cref="SystemTagTypes.DefaultColor"/>.</param>
        /// <returns>The created tag type entity, or null when the name is already taken</returns>
        Task<TagTypeEntity> CreateTagType(string name, string description, Type kind, byte priority = 0, Color? color = null);

        /// <summary>
        /// Determinants whenever there is a tag type with given name.
        /// </summary>
        /// <param name="name">Name of a tag type</param>
        /// <returns>True if there is a tag type with specified name</returns>
        Task<bool> TagTypeExists(string name);

        /// <summary>
        /// Attempts to find a TagType based on it's name property.
        /// </summary>
        /// <param name="name">Name of a tag type that should be found.</param>
        /// <returns>The matching tag type entity, or null when there is none</returns>
        Task<TagTypeEntity> FindTagType(string name);

        /// <summary>
        /// Updates TagType with corresponding name (primary key).
        /// TagType name is equivalent to TagModel#Name.
        /// </summary>
        /// <param name="name">Id of TagType aka TagModel#Name</param>
        /// <param name="color">New color of TagType</param>
        /// <param name="description">New description of TagType</param>
        /// <param name="priority">New priority of TagType</param>
        /// <returns>A task that will be completed after the operation is performed.</returns>
        Task UpdateTagType(string name, string description, byte? priority, Color? color);

        /// <summary>
        /// Removes tag type with given name along with all tags that were using this tag type.
        /// </summary>
        /// <param name="name">Id of TagType aka TagModel#Name</param>
        /// <returns>A task that will be completed after the operation is performed.</returns>
        Task DeleteTagType(string name);

    }

}
