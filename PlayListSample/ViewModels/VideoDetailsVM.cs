using PlayListSample.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayListSample.ViewModels
{
    public class VideoDetailsVM: BaseViewModel
    {
        private VideoItem videoItem;
        public VideoItem Video
        {
            get { return videoItem; }
            set { if (value != videoItem)
                {
                    videoItem = value;
                    OnPropertyChanged();
                }

            }

           
        }


    }
}
