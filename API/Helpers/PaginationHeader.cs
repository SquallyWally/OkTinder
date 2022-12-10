namespace API.Helpers
{
    /// <summary>
    /// Class <c>PaginationHeader</c>
    /// When the http response goes back to the clients,
    /// then they are going to be able to retrieve the pagination details
    /// from this header from the HTTP response
    /// </summary>
    public class PaginationHeader
    {
        public int CurrentPage { get; set; }

        public int TotalPages { get; set; }

        public int ItemsPerPage { get; set; }

        public int TotalItems { get; set; }

        public PaginationHeader(int currentPage, int itemsPerPage, int totalItems, int totalPages)
        {
            CurrentPage = currentPage;
            ItemsPerPage = itemsPerPage;
            TotalItems = totalItems;
            TotalPages = totalPages;
        }
    }
}