/// <binding BeforeBuild='default' Clean='clean' />
"use strict";

var bro = require("gulp-bro");
var changed = require("gulp-changed");
var concat = require("gulp-concat");
var cssmin = require("gulp-cssmin");
var del = require("del");
var gulp = require("gulp");
var gutil = require("gulp-util");
var htmlmin = require("gulp-htmlmin");
var merge = require("merge-stream");
var newer = require("gulp-newer");
var rename = require("gulp-rename");
var uglify = require("gulp-uglify");
var yargs = require("yargs");


var bundles = require("./bundles.json");


gulp.task("createJsBundles", function () {
    return getBundleTasks(bundles.jsBundles, uglify);
});

gulp.task("createCssBundles", function () {
    return getBundleTasks(bundles.cssBundles, cssmin);
});

gulp.task("copyFiles", function () {
    return merge(bundles.filesToCopy.map(function (bundle) {
        return gulp.src(bundle.inputFiles)
            .pipe(changed(bundle.targetFolder))
            .pipe(gulp.dest(bundle.targetFolder));
    }));
});

gulp.task("default", ["createJsBundles", "createCssBundles", "copyFiles"]);

gulp.task("clean", function () {
    return del("./wwwroot");
});



function getBundleTasks(bundles, minify) {
    var shouldMinify = yargs.argv && yargs.argv.production;

    return merge(bundles.map(function (bundle) {
        var browserifyOperation = bundle.packWithBrowserify ? bro : gutil.noop;
        var minifyOperation = shouldMinify ? minify : gutil.noop;
        var outputFile = shouldMinify ?
            getMinifiedOutputFile(bundle.outputFile) :
            bundle.outputFile;

        return gulp.src(bundle.inputFiles, { base: "." })
            .pipe(newer(outputFile))
            .pipe(browserifyOperation())
            .pipe(concat(outputFile))
            .pipe(minifyOperation())
            .pipe(gulp.dest("."));
    }));
}

function getMinifiedOutputFile(outputFile) {
    var extensionIndex = outputFile.lastIndexOf(".");
    var newExtension = ".min" + outputFile.substr(extensionIndex);

    return outputFile.substr(0, extensionIndex) + newExtension;
}

function validateBundles(bundles)
{
    var allBundles = (bundles.jsBundles || [])
        .concat(bundles.cssBundles || [])
        .concat(bundles.imageBundles || []);

    for (var i in allBundles)
    {
        var path = allBundles[i].outputFile || allBundles[i].targetFolder;

        if (path && !path.startsWith("wwwroot"))
        {
            throw new Error("Target path must be within the wwwroot folder: " + path);
        }
    }
}

validateBundles(bundles);