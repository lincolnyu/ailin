using AiLinWpf.Sources;

namespace AiLinWpf.Actions
{
    public class MediaInfoFiller
    {
        public MediaInfoFiller(ResourceList resourceList, MediaRepository repository)
        {
            ResourceList = resourceList;
            Repository = repository;
        }

        public ResourceList ResourceList { get; }
        public MediaRepository Repository { get; }

        public void Fill()
        {
            foreach (var resource in ResourceList.Resources)
            {
                var id = resource.Id;
                if (string.IsNullOrWhiteSpace(id)) continue;
                var item = Repository[id];
                if (item != null)
                {
                    // TITLE is not merged at the moment
                    if (item.Role != null)
                    {
                        resource.Role = item.Role;
                    }
                    if (item.Director != null)
                    {
                        resource.Director = item.Director;
                    }
                    if (item.Producer != null)
                    {
                        resource.Producer = item.Producer;
                    }
                    if (item.Playwright != null)
                    {
                        resource.Playwright = item.Playwright;
                    }
                    if (item.Producer != null)
                    {
                        resource.Producer = item.Producer;
                    }
                    if (item.Role != null)
                    {
                        resource.Role = item.Role;
                    }
                    if (item.Role != null)
                    {
                        resource.Role = item.Role;
                    }
                }
            }
        }
    }
}
