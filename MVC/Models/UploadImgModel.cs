using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVC_F83345.Models
{
    public class UploadImgModel
    {
        public int ImageID { get; set; }
        public string ImgName { get; set; }
        public string ImgPath { get; set; }
        public string EmailID { get; set; }

        public IEnumerable<HttpPostedFileBase> ImageFile { get; set; }
    }
}