namespace AgriMarket.Web.Areas.Client.ViewModels.Messaging;

internal class ConversationListViewModel
{
    public List<ConversationListItemViewModel> Conversations { get; set; } = [];
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
}
