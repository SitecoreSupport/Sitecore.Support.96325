
namespace Sitecore.Support.ContentSearch
{
  using System.Collections.Generic;
  using Sitecore.Data.Items;
  using Sitecore.ContentSearch;

  /// <summary>
  /// The sitecore item crawler.
  /// </summary>
  public class SitecoreItemCrawler : Sitecore.ContentSearch.SitecoreItemCrawler
  {
    internal SitecoreIndexableItem PrepareIndexableVersion(Item item, IProviderUpdateContext context)
    {
      var indexable = (SitecoreIndexableItem)item;
      var cloneBuiltinFields = (IIndexableBuiltinFields)indexable;
      cloneBuiltinFields.IsLatestVersion = item.Versions.IsLatestVersion();
      indexable.IndexFieldStorageValueFormatter = context.Index.Configuration.IndexFieldStorageValueFormatter;
      return indexable;
    }

    protected override void UpdateItemVersion(IProviderUpdateContext context, Item version, IndexEntryOperationContext operationContext)
    {
      SitecoreIndexableItem versionIndexable = this.PrepareIndexableVersion(version, context);

      this.Operations.Update(versionIndexable, context, context.Index.Configuration);

      this.UpdateClones(context, versionIndexable);

      this.UpdateLanguageFallbackDependentItems(context, versionIndexable, operationContext);
    }

    private void UpdateClones(IProviderUpdateContext context, SitecoreIndexableItem versionIndexable)
    {
      IEnumerable<Item> clones;
      using (new WriteCachesDisabler())
      {
        clones = versionIndexable.Item.GetClones(false);
      }


      foreach (var clone in clones)
      {
        var cloneIndexable = PrepareIndexableVersion(clone, context);

        if (!this.IsExcludedFromIndex(clone))
        {
          this.Operations.Update(cloneIndexable, context, context.Index.Configuration);
        }
      }
    }

  }
}
