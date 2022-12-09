using EasyAPICore;
using Microsoft.AspNetCore.Mvc;
using System;

namespace SampleWebAPI
{
    public class SampleService : IEasyAPI
    {
        public SampleService()
        {

        }

        [HttpPost]
        public BookReadDto Create(BookDto book)
        {
            return new BookReadDto
            {
                Id = new Random().Next(1, 100),
                Name = book.Name,
                Description = book.Description
            };
        }

        public BookReadDto Update(int id, BookDto book)
        {
            return new BookReadDto
            {
                Id = id,
                Name = book.Name,
                Description = book.Description
            };
        }

        public string Get(int id)
        {
            var dto = new BookReadDto
            {
                Id = id,
                Name = "Three body",
                Description = "The Trisolaran Dark Forest"
            };
            return dto.ToString();
        }

        public string Delete(int id)
        {
            var dto = new BookReadDto
            {
                Id = id,
                Name = "Three body",
                Description = "The Trisolaran Dark Forest"
            };
            return $"Delete {dto.ToString()} Success!" ;
        }
    }

    public class BookDto
    {
        /// <summary>
        /// Book Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Book Description
        /// </summary>
        public string Description { get; set; }
    }

    public class BookReadDto
    {
        /// <summary>
        /// Book Id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Book Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Book Description
        /// </summary>
        public string Description { get; set; }

        public override string ToString()
        {
            return $"Book ID:{Id},Book Name:{Name},Book Description:{Description}";
        }
    }
}