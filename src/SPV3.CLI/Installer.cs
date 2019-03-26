/**
 * Copyright (c) 2019 Emilian Roman
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would be
 *    appreciated but is not required.
 * 2. Altered source versions must be plainly marked as such, and must not be
 *    misrepresented as being the original software.
 * 3. This notice may not be removed or altered from any source distribution.
 */

using static System.Environment;
using static System.Environment.SpecialFolder;
using static System.IO.Compression.ZipFile;
using static System.IO.Directory;
using static System.IO.Path;
using static SPV3.CLI.Names;
using static SPV3.CLI.Names.Directories;
using static SPV3.CLI.Names.Files;

namespace SPV3.CLI
{
  /// <summary>
  ///   Installs packages to the filesystem.
  /// </summary>
  public static class Installer
  {
    /// <summary>
    ///   Installs packages from the source directory to the target directory on the filesystem.
    /// </summary>
    /// <param name="source">
    ///   Absolute path to directory containing the installation packages and manifest.
    /// </param>
    /// <param name="target">
    ///   Target directory to install the data from the packages to.
    /// </param>
    public static void Install(string source, string target)
    {
      /**
       * Conventionally, we expect the manifest to be located in the source directory along with the packages. This is
       * assumed by the fact that the COMPILER creates the manifest in the target directory that is provided to it.
       */
      var manifest = (Manifest) Combine(source, Files.Manifest);
      manifest.Load();

      /**
       * Installation is the reversal of the COMPILER routine: we get the data back from the DEFLATE packages, through
       * the use of the generated manifest, and inflate it to the provided target directory on the filesystem.
       */
      foreach (var package in manifest.Packages)
      {
        /**
         * To handle reinstall circumstances, and for the sake of being more defensive, we check if the package files
         * already exist on the filesystem. Should they exist, we will proceed with deleting them.
         */
        foreach (var entry in package.Entries)
        {
          var file = (File) Combine(target, package.Path, entry.Name);

          if (file.Exists())
            file.Delete();
        }

        /**
         * Given that the package filename on the filesystem is expected to match the package's name in the manifest, we
         * infer the package's path by combining the source with the aforementioned name.
         *
         * Additionally, if the current iteration's package represents a subdirectory, we can extract its contents to
         * the relevant subdirectory. This permits us to replicate the layout of the source directory that was given to
         * the COMPILER and - of course - properly install the files!
         */
        var packagePath = Combine(source, package.Name);
        var destination = Combine(target, package.Path);
        ExtractToDirectory(packagePath, destination);
      }

      /**
       * Delete potential manifest file at the target destination.
       */

      var potentialManifest = (File) Combine(target, manifest.Name);

      if (potentialManifest.Exists())
        potentialManifest.Delete();

      manifest.CopyTo(target);

      /**
       * Store the installation path in a text file that other applications can rely on for inferring the location where
       * SPV3 is installed.
       */
      var dataDirectory = Combine(GetFolderPath(ApplicationData), Data);

      CreateDirectory(dataDirectory);

      new File
      {
        Path = Combine(dataDirectory, InstallPath)
      }.WriteAllText(target);
    }
  }
}