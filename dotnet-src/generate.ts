/*
 * Copyright (c) 2020, salesforce.com, inc.
 * All rights reserved.
 * SPDX-License-Identifier: BSD-3-Clause
 * For full license text, see the LICENSE file in the repo root or https://opensource.org/licenses/BSD-3-Clause
 */

import path from "path";
import { generate } from "@commerce-apps/raml-toolkit";
import { registerHelpers, registerPartials, setupApis } from "./lib/utils";

const API_DIRECTORY = path.resolve(
  process.env.COMMERCE_SDK_INPUT_DIR || `${__dirname}/../dotnet-apis`
);

const OUTPUT_DIRECTORY = path.join(__dirname, "../dotnet-renderedTemplates/helpers");

registerHelpers();
registerPartials();

console.log(`Creating SDK for ${API_DIRECTORY}`);

const skipTestFiles = (src: string): boolean => !/\.test\.[a-z]+$/.test(src);

setupApis(API_DIRECTORY, path.resolve(`${__dirname}/../dotnet-renderedTemplates`))
  .then((apis: generate.ApiMetadata) => {
    console.log("Generate SDK - START");
    apis.render()
      .then(() => console.log("Generate SDK - FINISH"))
  });

setupApis(API_DIRECTORY, path.resolve(`${__dirname}/../../Salesforce.CommerceCloud`))
  .then((apis: generate.ApiMetadata) => {
    console.log("Generate SDK directly in Class LIbrary - START");
    apis.render()
      .then(() => console.log("Generate SDK directly in Class LIbrary - FINISH"))
  });
