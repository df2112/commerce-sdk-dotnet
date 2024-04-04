/*
 * Copyright (c) 2020, salesforce.com, inc.
 * All rights reserved.
 * SPDX-License-Identifier: BSD-3-Clause
 * For full license text, see the LICENSE file in the repo root or https://opensource.org/licenses/BSD-3-Clause
 */

import { commonParameterPositions } from "@commerce-apps/core";
import { amf, generate } from "@commerce-apps/raml-toolkit";
import { getResponsesFromPayload, getTypeFromPayload } from "@commerce-apps/raml-toolkit/lib/generate/utils";

/**
 * Given an individual type or an array of types in the format Array\<Foo | Baa\>
 * will return either the type prefixed by the namespace, or the Array with each type prefixed
 * eg. Array\<types.Foo | types.Baa\>
 *
 * @param content - to be parsed for types to prefix with a namespace
 * @param namespace - to be prefixed to types
 * @returns the content prefixed with the namespace
 */
export function addNamespace(content: string, namespace: string): string {
  // Not handling invalid content.
  if (!content) {
    throw new Error("Invalid content");
  }
  // Not handling invalid namespace.
  if (!namespace) {
    throw new Error("Invalid namespace");
  }

  // if the content is an array, extract all of the elements
  const matched = content.match(/^Array<(.*?)>$/);
  const arrayType = !!matched;
  const types = matched?.[1] || content;

  // Get a handle on individual types
  const typesToProcess = types.split("|");
  const namespaceTypes: string[] = [];

  // for each type
  typesToProcess.forEach((checkType) => {
    // trim the fat
    const actualType = checkType.trim();
    // check if there's an actual type present
    if (actualType === "") {
      throw new Error("Empty type found");
    }

    // void and object types don't get a namespace
    if (["void", "object"].includes(actualType.toLocaleLowerCase())) {
      namespaceTypes.push(actualType);
    } else {
      // everything else does
      namespaceTypes.push(`${namespace}.${actualType}`);
    }
  });

  // reconstruct the passed in type with the namespace
  const processedTypes = namespaceTypes.join(" | ");

  // Re-add Array if required
  if (arrayType) {
    return `Array<${processedTypes}>`;
  }
  return processedTypes;
}

/**
 * Certain characters need to be handled for TSDoc.
 *
 * @param str - The string to be formatted for TSDoc
 * @returns string reformatted for TSDoc
 */
export const formatForTsDoc = (str: string): string => {
  // Brackets are special to TSDoc and less than / greater than are interpreted as HTML
  const symbolsEscaped = str
    .toString()
    .replace(/([^\\])(["{}<>]+)/g, (m) => Array.from(m).join("\\"));
  // Double escaped newlines are replaced with real newlines
  const newlinesUnescaped = symbolsEscaped.replace(/\\n/g, "\n");
  // Double escaped tabs are replaced with a single space
  const tabsUnescaped = newlinesUnescaped.replace(/(\\t)+/g, " ");
  // Collapse leading whitespace of 4 or more to avoid triggering code block formatting
  const collapsedLeadingWhitespace = tabsUnescaped.replace(/\n {4,}/g, "\n   ");

  return collapsedLeadingWhitespace;
};

export const formatForCSharpXmlDoc = (str: string): string => {
  // Double escaped newlines are replaced with real newlines
  const newlinesUnescaped = str.toString().replace(/\\n/g, "\n");
  // Double escaped tabs are replaced with four spaces to match C# indentation
  const tabsUnescaped = newlinesUnescaped.replace(/\\t/g, "    ");
  // Split the string into lines, prepend each line with "/// ", and join them back together
  const linesWithComments = tabsUnescaped
    .split("\n")
    .map(line => "/// " + line)
    .join("\n");

  return linesWithComments;
};
/**
 * Checks if a path parameter is one of the set that are configurable at the client level
 *
 * @param property - The string name of the parameter to check
 * @returns true if the parameter is a common parameter
 */
export const isCommonPathParameter = (property: string): boolean =>
  property
    ? commonParameterPositions.pathParameters.includes(property.toString())
    : false;

/**
 * Checks if a query parameter is one of the set that are configurable at the client level
 *
 * @param property - The string name of the parameter to check
 * @returns true if the parameter is a common parameter
 */
export const isCommonQueryParameter = (property: string): boolean =>
  property
    ? commonParameterPositions.queryParameters.includes(property.toString())
    : false;

/**
 * Checks whether a trait is allowed to be used in the API. Only traits that comply
 * with API standards are allowed.
 *
 * Currently, the only known non-compliant trait is "offset-paginated". It does
 * not comply because it is not a camel case name. It can be safely ignored because
 * the compliant "OffsetPaginated" is also available. (The kebab case version has
 * not been removed to maintain backward compatibility.)
 *
 * @param trait - Trait to check
 * @returns true unless the trait's name is "offset-paginated"
 */
export const isAllowedTrait = (trait: amf.model.domain.Trait): boolean => {
  return /^[A-Za-z][A-Za-z0-9]*$/.test(trait.name.value());
};

/**
 * Checks if API name is a shopper API (vs admin/data API)
 *
 * @param name - name of API
 * @returns true if API is shopper API
 */
export const isShopperAPI = (name: string): boolean => {
  return name.toString().toLowerCase().startsWith("shop");
};
/**
 * Get data type of a property - DAVE C# version!
 *
 * @param property - An AMF property
 *
 * @returns data type, if defined in the property, the default type otherwise
 */
exports.getTypeFromPropertyCSharp = (property) => {
  const returnType = generate.handlebarsAmfHelpers.getTypeFromProperty(property);

  // switch (returnType) {
  //   case "any":
  //     return "dynamic";
  //   case "Array<any>":
  //     return "dynamic[]";
  //   case "Array<object>":
  //     return "object[]";
  //   case "Array<Filter>":
  //     return "Filter[]";
  //   case "Array<PropertyValueDefinition>":
  //     return "PropertyValueDefinition[]";
  //   case "Array<Sort>":
  //     return "Sort[]";
  //   case "Array<string>":
  //     return "string[]";
  //   case "boolean":
  //     return "bool";
  //   case "number":
  //     return "int";
  //   default:
  //     return returnType;
  // }

  if (returnType.startsWith("Array<") && returnType.endsWith(">")) {
    const elementType = returnType.slice(6, -1);  // Extract the element type

    switch (elementType) {
      case "any":
        return "dynamic[]";
      default:
        return elementType + "[]";  // Default to the element type followed by []
    }
  } else {
    switch (returnType) {
      case "any":
        return "dynamic";
      case "boolean":
        return "bool";
      case "number":
        return "double";
      case "object":
        return "Dictionary<string, dynamic>";
      default:
        return returnType;
    }
  }
};

/**
 * Get value from an AMF field.
 *
 * @param name - The field to extract the value from
 *
 * @returns The string of the value
 */
exports.getValueCSharp = (name) => {
  let value;

  if (typeof name?.value === "function") {
    value = name.value();
  }

  if (value == null) {
    return null;
  }

  if (value === "status") {
    value = "StatusEnum";
  }

  return value
    //.toLowerCase()
    .split('_')
    .map(word => word.charAt(0).toUpperCase() + word.slice(1))
    .join('');
};

exports.equalsCSharp = (arg1, arg2) => {
  return arg1 === arg2;
}

exports.upperCamelCaseCSharp = (name) => {
  return name.charAt(0).toUpperCase() + name.slice(1);
}

/**
 * Get the return type info of an operation.
 *
 * @param operation - An AMF operation
 *
 * @returns a string for the data type returned by the successful operation
 */
exports.getReturnTypeFromOperationCSharp = (
  operation: amf.model.domain.Operation
): string => {
  const okResponses = getResponsesFromPayload(operation);
  const dataTypes: string[] = [];

  okResponses.forEach((res) => {
    if (res.payloads.length > 0) {
      dataTypes.push(getTypeFromPayload(res.payloads[0]));
    } else {
      dataTypes.push("void");
    }
  });

  if (okResponses.length === 0) {
    dataTypes.push("void");
  }

  let dataTypesUnique = [...new Set(dataTypes)];

  return dataTypesUnique.join(" | ");
};

/**
 * Get the baseUri's variables from an AMF model.
 *
 * Note: TypeScript will not allow access to the data without the proper cast to
 * a WebApi type.
 *
 * @param property - A model from the the AMF parser
 *
 * @returns the base URI of the model
 */
exports.getBaseUriVariablesFromDocument = (property: amf.model.document.BaseUnitWithEncodesModel) => {
  if (property && property.encodes) {
    let variables = (property.encodes as amf.model.domain.WebApi).servers[0].variables;
    let result = variables.map((x) => x.name.value());
    return result;
  }

  return "";
};
