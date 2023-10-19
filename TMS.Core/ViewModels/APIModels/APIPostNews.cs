using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;
using TMS.Core.Utils;

namespace TMS.Core.ViewModels.APIModels
{
    public class APIPostNews
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public int StartDate { get; set; }
        public int EndDate { get; set; }
        public string PostBy { get; set; }
        public int CreationDate { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public int? CategoryId { get; set; }
        public int? Type { get; set; }
        public IEnumerable<APIItem> PostList{ get; set; }
        public class APIItem
        {
            public string TraineeItem { get; set; }
            public APIItem(GroupTrainee_Item group_item)
            {
               this.TraineeItem = group_item?.Trainee?.str_Staff_Id;

            }
        }

        public APIPostNews(Postnew postnew)
        {
            this.Id = postnew.Id;
            this.IsActive = postnew.IsActive ?? false;
            this.IsDeleted = postnew.IsDeleted ?? false;
            this.Title = postnew.Title;
            this.Image = (string.IsNullOrEmpty(postnew.Image) ? string.Empty : postnew.Image);
            this.StartDate = (int)DateUtil.ConvertToUnixTime(postnew.StartDate ?? DateTime.Now);
            this.EndDate = (int)DateUtil.ConvertToUnixTime(postnew.EndDate ?? DateTime.Now);
            this.PostBy = postnew.PostBy;
            this.CreationDate = (int)DateUtil.ConvertToUnixTime(postnew.CreationDate ?? DateTime.Now);
            this.Description = postnew.Description;
            this.Content = postnew.Content;
            this.CategoryId = postnew.CategoryID;
            this.Type = postnew.Type;
            if (postnew.PostNew_GroupTrainee.Any())
            {
                foreach (var item in postnew.PostNew_GroupTrainee)
                {
                    if(item.GroupTrainee.GroupTrainee_Item.Any())
                    {
                        this.PostList = item.GroupTrainee.GroupTrainee_Item.Select(a=> new APIItem(a));
                    }
                }
            }
        }
    }
}
