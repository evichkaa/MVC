using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVC_F83345.Models;
using System.IO;

namespace MVC_F83345.Controllers
{
    public class ImageController : Controller
    {
        [HttpGet]
        public ActionResult Add()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult Add(Image Img)
        {
            foreach (var file in Img.ImageFile)
            {
                var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                string Extension = Path.GetExtension(file.FileName);
                fileName = fileName + DateTime.Now.ToString("yymmssfff") + Extension;
                Img.ImgPath = "~/Images/" + fileName;
                fileName = Path.Combine(Server.MapPath("~/Images/"), fileName);
                file.SaveAs(fileName);
                using (DatabaseEntities db = new DatabaseEntities())
                {
                    Img.EmailID = HttpContext.User.Identity.Name;
                    db.Images.Add(Img);
                    db.SaveChanges();
                    ModelState.Clear();
                }
            }
                return View();
        }

        [Authorize]
        [HttpGet]
        public ActionResult View(int ID)
        {
            Image Img = new Image();
            using (DatabaseEntities db = new DatabaseEntities())
            {
                Img = db.Images.Where(x => x.ImageID == ID).FirstOrDefault();
            }
            return View(Img);
        }

        [Authorize]
        [HttpGet]
        public FileResult DownloadImg(string p)
        {
            var type = Path.GetExtension(p);
            string name = "";
            using (DatabaseEntities de = new DatabaseEntities())
            {
                var img = de.Images.Where(a => a.ImgPath == p).FirstOrDefault();
                name = img.ImgName + type;
            }
            return File(p, type, name);
        }

        [Authorize]
        [HttpGet]
        public ActionResult ViewList()
        {
            List<UploadImgModel> ImgList = new List<UploadImgModel>();
            using (DatabaseEntities db = new DatabaseEntities())
            {
                foreach (var image in db.Images) {
                    if (image.EmailID == HttpContext.User.Identity.Name || HttpContext.User.Identity.Name == "vpm97@abv.bg") {
                        UploadImgModel ig = new UploadImgModel();
                        ig.ImageID = image.ImageID;
                        ig.ImgName = image.ImgName;
                        ig.ImgPath = image.ImgPath;
                        ig.EmailID = image.EmailID;
                        ImgList.Add(ig);
                    }
                }
            }
            return View(ImgList);
        }

        [Authorize]
        [HttpGet]
        public ActionResult DeleteImg(string p)
        {
            string message = "";
            using (DatabaseEntities dm = new DatabaseEntities())
            {
                var ima = dm.Images.Where(a => a.ImgPath == p).FirstOrDefault();
                if (ima != null)
                {
                    if (ima.EmailID == HttpContext.User.Identity.Name)
                    {
                        if (System.IO.File.Exists(Request.MapPath(p)))
                        {
                            dm.Images.Remove(ima);
                            dm.SaveChanges();
                            System.IO.File.Delete(Request.MapPath(p));
                        }
                    }
                }
            }
            ViewBag.Message = message;
            return Redirect("ViewList");
        }
    }
}