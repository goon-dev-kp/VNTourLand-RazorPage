//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace DAL.Models
//{
//    public class AddOnOption
//    {
//        public Guid AddOnOptionId { get; set; }
//        public string Name { get; set; }
//        public string Description { get; set; }
//        public decimal Price { get; set; }
//        public Guid? TourId { get; set; }
//        public virtual Tour Tour { get; set; }

//        public virtual ICollection<OptionOnTour> OptionOnTours { get; set; } = new HashSet<OptionOnTour>();

//    }
//}
