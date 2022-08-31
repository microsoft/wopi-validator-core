module.exports = function (grunt) {
	const nugetOutFolder = "./";
	const solutionPath = "WopiValidator.sln";
	const projectPath = "src/WopiValidator.Core/WopiValidator.Core.csproj";
    const publishPath = "bin/Release/PublishOutput/WopiValidator.Core";

	grunt.initConfig({
		pkg: grunt.file.readJSON("package.json"),
		clean: {			
			basic: [
				"TestResults/",
                "Source/**/bin/**",
				"Source/**/obj/**"
			],
			options: {
				folders: true
			}
		},
		
		shell: {
			dotnetRestore: {
				command:
					`dotnet restore ${solutionPath}`
			},
			dotnetPublish: {
				command:
					`dotnet publish ${projectPath} --configuration Release -o ${publishPath}`
			},
			dotnetBuildDebug: {
				command:
					`dotnet build ${solutionPath} --configuration Debug`
			},
			dotnetBuildRelease: {
				command:
					`dotnet build ${solutionPath} --configuration Release`
			},
			dotnetCleanRelease: {
				command:
					`dotnet clean ${solutionPath} --configuration Release`
			},
			dotnetTest: {
				command:
                    `dotnet test ${solutionPath} --logger:trx;LogFileName=TestResultVSTest.trx;verbosity=minimal`
			}
		},
		nugetpush: {
			packs: {
				src: [nugetOutFolder + "/*.nupkg", "!" + nugetOutFolder + "*.symbols.nupkg"],
				options: {
					apiKey: process.env["ApiKey"],
					configFile: "Source/nuget.config",	// keep this to avoid pushing packages from dev pcs
					source: "https://www.myget.org/F/geckoprivate/api/v2/package"
				}
			},
			symbols: {
				src: nugetOutFolder + "/*.symbols.nupkg",
				options: {
					apiKey: process.env["ApiKey"],
					configFile: "Source/nuget.config",	// keep this to avoid pushing symbols from dev pcs
					source: "https://www.myget.org/F/geckoprivate/symbols/api/v2/package"
				}
			}
		},
		copy: {
			dist: {
				files: [
					{
						expand: true,
						cwd: "Documentation",
						src: ["**"],
						dest: "dist/"
					},
					{
						expand: true,
						flatten: true,
						cwd: "Source",
						src: ["**/*.nupkg"],
						dest: "dist/NuGet/"
					}
				]
			}
		}
	});

	grunt.loadNpmTasks("grunt-nuget");
	grunt.loadNpmTasks("grunt-contrib-clean");
	grunt.loadNpmTasks("grunt-shell");
	grunt.loadNpmTasks("grunt-contrib-copy");

	// Default task(s).
	grunt.registerTask("restore", ["shell:dotnetRestore"]);
	grunt.registerTask("cleanAll", ["shell:dotnetCleanRelease", "clean"]);
	grunt.registerTask("buildDebug", ["shell:dotnetBuildDebug"]);
	grunt.registerTask("buildRelease", ["shell:dotnetBuildRelease"]);
	grunt.registerTask("test", ["shell:dotnetTest"]);
	grunt.registerTask("push", ["nugetpush"]);
	grunt.registerTask("dist", ["copy:dist"]);
	grunt.registerTask("default", ["cleanAll", "buildRelease", "test", "dist"]);
};
