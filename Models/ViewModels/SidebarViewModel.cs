namespace SchoolProject.Models.ViewModels
{
    public class SidebarItem
    {
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
        public string? Subtitle { get; set; }
    }

    public class SidebarSection
    {
        public string Heading { get; set; } = "";
        public List<SidebarItem> Items { get; set; } = new();
    }

    public class SidebarViewModel
    {
        public List<SidebarSection> Sections { get; set; } = new();
    }
}