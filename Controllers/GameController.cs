using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Reversi.Models;

namespace Reversi.Controllers
{
    public class GameController : Controller
    {
//        private BoardViewModel boardModel;

        public ActionResult New()
        {
            BoardViewModel board = new BoardViewModel();
            return RedirectToAction("Play"); //, new { id = board.BoardId });
        }

        public ActionResult Play()
        {
//            boardModel = BoardViewModel.GetById(id);
            ViewBag.Title = "Jo's Othello Board #1";
            return View(new BoardViewModel());
        }

        public ActionResult Reversi()
        {
            return View();
        }

    }
}