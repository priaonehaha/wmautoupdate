﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Net;

namespace WmAutoUpdate
{
  class TransferManager
  {
    public delegate void TransferProgress(int progress);

    private TransferProgress transferProgressDelegate;

    public bool downloadFile(String url, out Stream s, String path, TransferProgress del)
    {
      this.transferProgressDelegate = del;
      byte[] buffer = new byte[4096];
      FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);

      WebRequest wr = WebRequest.Create(url);
      wr.Proxy = System.Net.GlobalProxySelection.Select;
      try
      {
        using (WebResponse response = wr.GetResponse())
        {
          using (Stream responseStream = response.GetResponseStream())
          {
            int count = 0;
            int dataRead = 0;
            do
            {
              count = responseStream.Read(buffer, 0, buffer.Length);
              fileStream.Write(buffer, 0, count);

              float progress = ((float)dataRead / (float)response.ContentLength) * 100.0f;
              OnProgress((int)progress);

              dataRead += count;
            } while (count != 0);
          }
        }
      }
      catch (WebException wex)
      {
        Logger.Instance.log("No Connection to update server...");
        fileStream.Close();
        s = null;
        return false;
      }
      s = fileStream;
      //      fileStream.Close();
      return true;
    }

    // By Default, create an OnXXXX Method, to call the Event
    protected void OnProgress(int progress)
    {
      if (transferProgressDelegate != null)
      {
        transferProgressDelegate(progress);
      }
    }

  }
}
