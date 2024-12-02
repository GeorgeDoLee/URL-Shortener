﻿namespace URL_Shortener.Models.Entities
{
    public class Url
    {
        public int Id { get; set; }
        public required string OriginalUrl { get; set; }
        public required string ShortenedUrl { get; set; }
        public required DateTime ExpirationDate { get; set; }
    }
}
