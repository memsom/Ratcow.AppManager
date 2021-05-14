/*  
 *  MIT License
 *  
 *  Copyright 2021 Rat Cow Software and Matt Emson. 
 *
 *  Permission is hereby granted, free of charge, to any person obtaining a copy
 *  of this software and associated documentation files (the "Software"), to deal
 *  in the Software without restriction, including without limitation the rights
 *  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 *  copies of the Software, and to permit persons to whom the Software is
 *  furnished to do so, subject to the following conditions:
 *  
 *  The above copyright notice and this permission notice shall be included in all
 *  copies or substantial portions of the Software.
 *  
 *  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 *  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 *  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 *  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 *  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 *  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 *  SOFTWARE.
 *  
 */

using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Android.Text;
using AndroidX.Core.Content;
using Java.IO;
using Java.Lang;
using Java.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace RatCow.AppManager
{
    public class AppManagerAndroid : AppManager
    {
        readonly Context context = null;

        // this needs to be called 
        public static void Init(Context context)
        {
            AppManager.Instance = new AppManagerAndroid(context);
        }

        AppManagerAndroid(Context context)
        {
            this.context = context;
        }

        // applies the filter... this is probably overkill
        IEnumerable<ApplicationInfo> GetApplicationInfoList(IList<ApplicationInfo> apps, string filter = null)
        {
            if (!string.IsNullOrEmpty(filter))
            {
                return apps.Where(x => x.PackageName.Contains(filter));
            }
            else
            {
                return apps;
            }
        }

        // get a list of all installed apps
        public override List<InstalledAppData> GetIntalledApps(string filter = null)
        {
            var inApps = new List<InstalledAppData>();
            IList<ApplicationInfo> apps = context.PackageManager.GetInstalledApplications(PackageInfoFlags.MatchAll);

            foreach (var app in GetApplicationInfoList(apps, filter))
            {
                var packageInfo = context.PackageManager.GetPackageInfo(app.PackageName, 0);
                inApps.Add(new InstalledAppData(app.LoadLabel(context.PackageManager), app.PackageName, packageInfo.LongVersionCode, packageInfo.VersionName));
            }
            return inApps;
        }


        // Does some Android magic to make the acces work.
        static Uri GetPathUri(Context context, string filePath)
        {
            Uri uri;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                string packageName = context.PackageName;
                uri = FileProvider.GetUriForFile(context, packageName + ".provider", new File(filePath));
            }
            else
            {
                uri = Uri.FromFile(new File(filePath));
            }
            return uri;
        }

        // gets an external path
        public override string GetNativePath(string directory)
        {
            var dir = System.IO.Path.Combine(
                Environment.ExternalStorageDirectory.Path,
                directory);
            System.IO.Directory.CreateDirectory(dir);
            return dir;
        }

        // gets a file given a url
        // puts in directory/filename
        public override async Task<bool> InstallRemoteFile(string link, string directory, string fileName)
        {
            var path = System.IO.Path.Combine(GetNativePath(directory), fileName);

            return await InstallRemoteFile(link, path);
        }

        // gets a file given a url
        // puts in path
        public override async Task<bool> InstallRemoteFile(string link, string path)
        {
            if (context == null)
            {
                throw new AppServiceNotInitializedException("You must call Init() before using the AppService");
            }
            else
            {
                return await Task.Run(() =>
                {
                    // path will just be a filename


                    if (System.IO.File.Exists(path))
                    {
                        System.IO.File.Delete(path);
                    }

                    var url = new Java.Net.URL(link);
                    var connection = (HttpURLConnection)url.OpenConnection();
                    connection.RequestMethod = "GET";
                    connection.DoOutput = true;
                    connection.Connect();

                    using var input = url.OpenStream();
                    using var fos = new FileOutputStream(path);

                    var buffer = new byte[1024];
                    int length = 0;
                    while ((length = input.Read(buffer, 0, 1024)) > 0)
                    {
                        fos.Write(buffer, 0, length);
                    }
                    fos.Close();
                    input.Close();

                    return Install(path);
                });
            }
        }

        // installs an apk that was already downloaded, given an absolute path
        public override bool Install(string filePath)
        {
            if (context == null)
            {
                throw new AppServiceNotInitializedException("You must call Init() before using the AppService");
            }
            else
            {
                try
                {
                    if (TextUtils.IsEmpty(filePath))
                    {
                        return false;
                    }

                    var file = new File(filePath);
                    if (!file.Exists())
                    {
                        return false;
                    }

                    Intent intent = new Intent(Intent.ActionView)
                        .SetDataAndType(GetPathUri(context, filePath), "application/vnd.android.package-archive")
                        .SetFlags(ActivityFlags.NewTask) // without this flag android returned a intent error!
                        .AddFlags(ActivityFlags.GrantReadUriPermission);
                    context.StartActivity(intent);
                }
                catch (Exception e)
                {
                    e.PrintStackTrace();
                    return false;
                }
                catch (Error error)
                {
                    error.PrintStackTrace();
                    return false;
                }
                return true;
            }
        }

        // uninstalls an apk
        public override void Uninstall(string packageName)
        {
            if (context == null)
            {
                throw new AppServiceNotInitializedException("You must call Init() before using the AppService");
            }
            else
            {
                Intent intent = new Intent(Intent.ActionDelete)
                    .SetData(Uri.Parse($"package:{packageName}"))
                    .SetFlags(ActivityFlags.NewTask);
                context.StartActivity(intent);
            }
        }
    }
}