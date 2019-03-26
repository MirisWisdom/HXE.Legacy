<!--
 Copyright (c) 2019 Emilian Roman
 
 This software is provided 'as-is', without any express or implied
 warranty. In no event will the authors be held liable for any damages
 arising from the use of this software.
 
 Permission is granted to anyone to use this software for any purpose,
 including commercial applications, and to alter it and redistribute it
 freely, subject to the following restrictions:
 
 1. The origin of this software must not be misrepresented; you must not
    claim that you wrote the original software. If you use this software
    in a product, an acknowledgment in the product documentation would be
    appreciated but is not required.
 2. Altered source versions must be plainly marked as such, and must not be
    misrepresented as being the original software.
 3. This notice may not be removed or altered from any source distribution.
-->

SPV3 CLI SOURCE CODE
====================

This section contains the implementation source code for the SPV3 CLI.
As mentioned in this repository's README, the CLI allows:

-   LOADING

    -   automatic HCE/SPV3 player profile detection
    -   automatic resuming of the SPV3 campaign progress
    -   automatic detection & launching the HCE executable
    -   verification of the SPV3 assets/maps/executables

-   COMPILING

    -   compressing SPV3 data to re-distributable packages
    -   generating a manifest for the installer & loader

-   INSTALLING

    -   (re)installing compressed packages to the filesystem
    -   storing the chosen installation path in a standard file

This document serves as a high-level documentation for the source.
Specific documentation can be found within the source code itself.

LOADER INFORMATION
------------------

The loading procedure, in a nutshell, is as follows:

1.  verify the main SPV3 assets on the filesystem, by comparing their
    file lengths against the lengths specified in the manifest; and

2.  resume of the SPV3 campaign, through heuristic detection of the
    player's profile and inference of the mission & difficulty; and

3.  check for an override file, for the purpose of easy debugging of
    SPV3 by forcefully changing any imposed loader/HCE/OS/Chimera
    settings; and

4.  invoke of the HCE executable within the working directory, with
    arguments being passed onto the invoked executable process.

All of the steps above are conducted by the Kernel class, which serves
as the highest-level abstraction of the aforementioned procedure.

COMPILER INFORMATION
--------------------

The compiler handles the preparation of SPV3 data for distribution and
installation. It accomplishes this by DEFLATE-compressing all of the
subdirectories in the source directory to individual packages in the
target directory.

It also adds an entry for the subdirectory in the manifest: this entry
declares the relative path of the subdirectory to the source directory,
the name of the package, and the list of files the subdirectory
contains.

The compilation procedure, in a nutshell, is as follows:

1.  create a list containing:

    -   the specified source directory, and
    -   any subdirectories in the source directory.

2.  for each subdirectory, create a DEFLATE package in the specified
    target directory, and an entry in the manifest with:

    -   the package's name on the filesystem (hex.bin convention) and;
    -   the path the subdirectory resides in, relative to the source.

3.  for each file within the respective subdirectory, add an entry to
    the package on the filesystem, add an entry for it in the manifest
    containing:

    -   the filename (no paths) on the filesystem; and
    -   the file size (byte length) on the filesystem.

4.  save of the manifest data in DEFLATE format to the target directory,
    for the aforementioned distribution and installation.

For further information on the anatomy of a manifest file, please refer
to the manifest.txt documentation in the doc directory.

INSTALLER INFORMATION
---------------------

The installer handles the extraction of data from the DEFLATE packages -
which have been created by the compiler - to the filesystem. It relies
on the generated manifest for inferring which packages are installable,
and loops through each package to install it to the filesystem.

Additionally, it also verifies if files within packages already exist at
the target destination, and if it finds the file, then it will delete it
before installing the package. This is done to practically handle
reinstall scenarios.

To permit other programs to determine the path which SPV3 is installed
to, the installer also creates a text file which contains the absolute
path of the directory which SPV3 is installed to.

The installation procedure, in a nutshell, is as follows:

1.  load the manifest file from the provided source directory; and

2.  start iterating through each package in the loaded manifest; and

3.  for each file within the iteration's package, check if the file
    exists at the target destination, and delete it upon confirming its
    existence; and

4.  infer the destination path for the extracted files, then extract the
    package's data to it; and

5.  check if a manifest already exists at the target destination, and
    delete it upon confirming its existence; and

6.  copy the manifest file from the source directory the target
    directory, to allow the loader to verify the assets at runtime; and

7.  create a text file containing the target directory's absolute path,
    to permit other applications to infer SPV3's installation path.

For further information on the aforementioned installation text file,
please refer to the installer.txt documentation in the doc directory.

FILESYSTEM OBJECTS
------------------

A significant portion of source code focuses on representing files as
objects. This section seeks to outline each object and the respective
file it represents.

All of the types listed in the following table are inheritors of the
File type:

  ---------------------------------------------------------------------
  Object        Description
  ------------- -------------------------------------------------------
  Executable    Represents the haloce.exe executable. Permits
                invocation of the executable on the filesystem. Common
                HCE executable arguments are exposed as properties.

  Initiation    Represents the OpenSauce initc.txt file. For more
                information on the file itself, please refer to
                doc/initc.txt. This object permits the saving of its
                properties to initc.txt values, including the mission,
                difficulty, post-processing settings, and some
                miscellaneous toggles.

  LastProfile   Represents the lastprof.txt file, which is used in the
                kernel's auto-detection mechanism. This object permits
                the retrieval of the last played profile name.

  Override      Represents the overrides.XML file, which is used for
                debugging purposes by forcefully overriding any imposed
                or default settings by the loader, HCE, OpenSauce or
                Chimera.

  Progress      Represents the savegame.bin file, which contains the
                player's checkpoint data. This object infers and
                exposes the mission & difficulty, which in turn can be
                saved using the Initiation object for campaign
                resuming.

  OpenSauce     Represents the OS\_Settings.User.xml, which contains
                the OpenSauce user configuration data. The loader
                exposes all of the available options as object
                properties, for programmable editing of the OpenSauce
                configuration.

  Profile       Represents the blam.sav binary, which contains HCE
                profile information and configuration data. The object
                permits loading, editing, and saving this data.
                Additionally, it also automatically forges the hash of
                the blam.sav upon saving, to ensure that HCE accepts
                the edited binary.
  ---------------------------------------------------------------------

Documentation on any of the aforementioned files can be found in the doc
directory within this repository.