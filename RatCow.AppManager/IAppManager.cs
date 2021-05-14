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

using System.Collections.Generic;
using System.Threading.Tasks;

namespace RatCow.AppManager
{
    public interface IAppManager
    {
        // get a list of all installed apps
        List<InstalledAppData> GetIntalledApps(string filter = null);

        // gets a native path
        string GetNativePath(string directory);

        // gets a file given a url
        // puts in directory/filename
        Task<bool> InstallRemoteFile(string link, string directory, string fileName);

        // gets a file given a url
        // puts in path
        Task<bool> InstallRemoteFile(string link, string path);

        // installs an apk that was already downloaded, given an absolute path
        bool Install(string filePath);

        // uninstalls an apk
        void Uninstall(string packageName);
    }
}
