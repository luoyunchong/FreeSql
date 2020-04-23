﻿using FreeSql;
using FreeSql.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace aspnetcore_transaction.Controllers
{
    [ApiController]
    [Route("")]
    public class HomeController : ControllerBase
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        //[Transactional]
        virtual public object Get([FromServices] BaseRepository<Song> repoSong, [FromServices] BaseRepository<Detail> repoDetail, [FromServices] SongRepository repoSong2,
            [FromServices] SongService serviceSong)
        {
            serviceSong.Test();
            return "111";
        }
    }

    public class SongService
    {
        BaseRepository<Song> _repoSong;
        BaseRepository<Detail> _repoDetail;
        SongRepository _repoSong2;

        public SongService(BaseRepository<Song> repoSong, BaseRepository<Detail> repoDetail, SongRepository repoSong2)
        {
            _repoSong = repoSong;
            _repoDetail = repoDetail;
            _repoSong2 = repoSong2;
        }

        [Transactional(Propagation = Propagation.Nested)] //sqlite 不能嵌套事务，会锁库的
        public virtual void Test()
        {
            _repoSong.Insert(new Song());
            _repoDetail.Insert(new Detail());
            _repoSong2.Insert(new Song());
        }
    }

    public class SongRepository : DefaultRepository<Song, int>
    {
        public SongRepository(UnitOfWorkManager uowm) : base(uowm?.Orm, uowm) { }
    }

    public class Song
    {
        [Column(IsIdentity = true)]
        public int Id { get; set; }
        public string Title { get; set; }
    }
    public class Detail
    {
        [Column(IsIdentity = true)]
        public int Id { get; set; }

        public int SongId { get; set; }
        public string Title { get; set; }
    }
}
