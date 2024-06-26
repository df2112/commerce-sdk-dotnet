/*
 * Copyright (c) 2021, salesforce.com, inc.
 * All rights reserved.
 * SPDX-License-Identifier: BSD-3-Clause
 * For full license text, see the LICENSE file in the repo root or https://opensource.org/licenses/BSD-3-Clause
 */

import { generate, download } from "@commerce-apps/raml-toolkit";
import path from "path";
import * as helpers from "./templateHelpers";
import extendHandlebars from "handlebars-helpers";
import { readJsonSync } from "fs-extra";

const PROJECT_ROOT = path.join(__dirname, "../..");
const TEMPLATE_DIRECTORY = path.join(PROJECT_ROOT, "dotnet-templates");
const HELPERS_TEMPLATE_DIRECTORY = path.join(
  PROJECT_ROOT,
  "dotnet-src",
  "static",
  "helperTemplates"
);
const PACKAGE_JSON = path.join(PROJECT_ROOT, "package.json");

//////// HELPER REGISTRATION ////////
// eslint-disable-next-line @typescript-eslint/naming-convention
const Handlebars = generate.HandlebarsWithAmfHelpers;
extendHandlebars({ handlebars: Handlebars });

/**
 * Register the custom helpers defined in our pipeline
 */
export function registerHelpers(): void {
  for (const helper of Object.keys(helpers)) {
    Handlebars.registerHelper(helper, helpers[helper]);
  }
}

/**
 * Register any customer partials we have in our pipeline
 */
export function registerPartials(): void {
  generate.registerPartial(
    "dtoPartial-cs",
    path.join(TEMPLATE_DIRECTORY, "dtoPartial.cs.hbs")
  );
  generate.registerPartial(
    "operationsPartial-cs",
    path.join(TEMPLATE_DIRECTORY, "operations.cs.hbs")
  );
}

function addTemplates(
  apis: generate.ApiMetadata,
  outputBasePath: string
): generate.ApiMetadata {

  const helperTemplateFileNames = ["shopperCustomer"];
  helperTemplateFileNames.forEach((name: string) => {
    let namePascalCase = name.charAt(0).toUpperCase() + name.slice(1);
    apis.addTemplate(
      path.join(HELPERS_TEMPLATE_DIRECTORY, `${name}.cs.hbs`),
      path.join(outputBasePath, "Helpers", `${namePascalCase}.cs`)
    );
  });

  apis.children.forEach((child: generate.ApiMetadata) => {
    child.children.forEach(async (api) => {
      api.addTemplate(
        path.join(TEMPLATE_DIRECTORY, "ClientInstance.cs.hbs"),
        path.join(
          outputBasePath,
          child.name.upperCamelCase,
          api.name.upperCamelCase,
          `${api.name.upperCamelCase}.cs`
        )
      );
    });
  });
  return apis;
}

/**
 * Primary driver, loads the apis and templates associated with those apis.
 *
 * @param inputDir - Directory for input
 * @param outputDir - Directory for output
 * @returns - The a promise to have the ApiMetaData tree ready to be rendered
 */
export async function setupApis(
  inputDir: string,
  outputDir: string
): Promise<generate.ApiMetadata> {
  let apis = generate.loadApiDirectory(inputDir);
  // SDK version is not API metadata, so it is not included in the file, but it
  // is necessary for generating the SDK (as part of the user agent header).
  apis.metadata.sdkVersion = await readJsonSync(PACKAGE_JSON).version;
  await apis.init();

  apis = addTemplates(apis, outputDir);
  return apis;
}

/**
 * Updates a set of APIs for an api family and saves it to a path.
 *
 * NOTE: Coverage passes without this function being covered.
 *  We should have some followup to figure out how to cover it.
 *  Ive spent hours trying to mock download
 *
 * @param apiFamily - Api family to download
 * @param deployment - What deployment to build for
 * @param rootPath - Root path to download to
 * @returns a promise that we will complete
 */
export async function updateApis(
  apiFamily: string,
  deployment: RegExp,
  rootPath: string
): Promise<void> {
  try {
    const apis = await download.search(
      `category:"CC API Family" = "${apiFamily}"`,
      deployment
    );
    await download.downloadRestApis(apis, path.join(rootPath, apiFamily));
  } catch (e) {
    console.error(e);
  }
}
