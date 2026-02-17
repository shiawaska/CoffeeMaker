# Window Snapper

S`imple overview of use/purpose.`

## Description

An in-depth paragraph about your project and overview of use.

## Getting Started

Window Snapper is a C# application designed to enhance window management on Windows 11 systems. 
It allows users to efficiently organize and snap application windows across one or more monitors,
including setups with multiple rows. The project leverages C# 14 and .NET 10, ensuring modern language
features and runtime performance, though specific .NET versioning is not a strict requirement. 
To operate, Window Snapper requires a configuration file that defines the user's desired window layouts 
and behaviors. If this file is missing, the application will automatically generate a template configuration 
in the specified location, simplifying initial setup. The tool is tailored for users seeking advanced control 
over their workspace, supporting diverse hardware configurations and providing a flexible, user-driven experience.

### Dependencies

- Only tested on Windows 11
- Future plans for Linux support
- Requires .NET 10
- Requires C# 14

### Installing

1. Clone or download the repository from GitHub.
1. Open the project in your preferred C# IDE (such as JetBrains Rider or Visual Studio).
1. Build the project to generate the executable (.exe) file. This can typically be done by selecting the Build option in your IDE or running dotnet build from the terminal.
1. The compiled .exe will be located in the bin\Debug\net10.0\ (or similar) directory.
1. Place the configuration file in the designated location. If it does not exist, the application will generate a template file on first run. This template includes example commands to help you understand how to configure the config file's information.


### Executing program

* How to run the program
* 
```
dotnet run 
```

## Help

1. For further assistance or to view all available arguments, run the program with the --help argument to see troubleshooting options and usage instructions.
```
Run the program with the --help argument to see troubleshooting options and usage instructions.
```

## Authors

Contributors names and contact info

Carlton Brown

## Version History

* 0.1
    * Initial Release

## License

None 

## Acknowledgments

Inspiration, code snippets, etc.
* [awesome-readme](https://github.com/matiassingers/awesome-readme)
* [PurpleBooth](https://gist.github.com/PurpleBooth/109311bb0361f32d87a2)
* [dbader](https://github.com/dbader/readme-template)
* [zenorocha](https://gist.github.com/zenorocha/4526327)
* [fvcproductions](https://gist.github.com/fvcproductions/1bfc2d4aecb01a834b46)