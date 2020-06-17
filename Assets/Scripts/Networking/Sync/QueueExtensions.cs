using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace Networking
{
    public static class QueueExtensions
    {
        public static Target Dequeue<Target>(this Queue<object> queue)
        {
            return (Target)queue.Dequeue();
        }
    }
}