/*
 * MODUS ASSUMPTION 2022
 */

(function() {
    let endpoints;
    let supplementalDocs;

    /**
     * DOM utilities.
     */
    const Dom = {
        /**
         * Creates a header element.
         * @param {String} title The title of this header.
         * @returns {HTMLDivElement}
         */
        createHeader: function(title) {
            const hdrEl = document.createElement('div');
            hdrEl.classList.add('header');
            hdrEl.textContent = title;
            return hdrEl;
        },
        /**
         * Creates a table column.
         * @param {String} caption The caption for this column.
         * @param {Number} width The column width.
         * @returns 
         */
        createTableCol: function(caption, width = null) {
            const tableCol = document.createElement('th');
            tableCol.textContent = caption;

            if(width != null)
                tableCol.style.width = (typeof width == 'number') ? `${width}px` : width;
            
            return tableCol;
        }
    }

    /**
     * Gets metadata for the specified route.
     * @param {"get"|"post"|"put"|"patch"|"delete"} method The HTTP method of the route.
     * @param {String} path The path of the route.
     */
    function getRouteMeta(method, path) {
        return supplementalDocs.find((x) => (x.method == method) && (x.path == path)) || {};
    }

    /**
     * Gets the request body for the specified route.
     * @param {"get"|"post"|"put"|"patch"|"delete"} method The HTTP method of the route.
     * @param {String} path The path of the route.
     */
    function getRequestBody(method, path) {
        const route = getRouteMeta(method, path);

        if(!route)
            return null;

        const body = route.requestBody;

        if(!body)
            return null;
        
        switch(body.type) {
            default:
                throw new Error("Unknown type");
            case "string":
                return body.data;
            case "json":
                return JSON.stringify(body.data, null, 4);
        }
    }

    /**
     * Gets the response body for the specified route.
     * @param {Number} code The HTTP response code.
     * @param {"get"|"post"|"put"|"patch"|"delete"} method The HTTP method of the route.
     * @param {String} path The path of the route.
     */
    function getResponseBody(code, method, path) {
        const route = getRouteMeta(method, path);

        if((!route) || (!route.responseBodies))
            return null;

        const body = route.responseBodies[`${code}`];

        if(!body)
            return null;
        
        switch(body.type) {
            default:
                throw new Error("Unknown type");
            case "string":
                return body.data;
            case "json":
                return JSON.stringify(body.data, null, 4);
        }
    }

    /**
     * Fetches additional documentation.
     */
    async function fetchSupplementalDocs() {
        const req = await fetch('data/descriptions.json');
        supplementalDocs = await req.json();
    }

    /**
     * Entry point.
     */
    window.getDocs = async function() {
        try {
            const epReq = await fetch('data/endpoints.json');
            endpoints = await epReq.json();
            await fetchSupplementalDocs();
        } catch(e) {
            alert(`{e}: Cannot fetch endpoints.`);
            return;
        }

        // Iterate each path
        const paths = Object.keys(endpoints.paths);
        
        // Get api view container
        const apiViewCt = document.querySelector("main>.apis");

        for(let pathName of paths) {
            const methodNames = Object.keys(endpoints.paths[pathName]);

            // Go through each method "post", "get", etc.
            for(let methodName of methodNames) {
                const methodObj = endpoints.paths[pathName][methodName];
                const routeMeta = getRouteMeta(methodName, pathName);

                // Create an API box
                const apiBox = document.createElement('div');
                apiBox.classList.add('box', methodName);
                
                // Create endpoint container element
                const epContainerEl = document.createElement('div');
                epContainerEl.classList.add('endpoint');

                const epMethodEl = document.createElement('div');
                epMethodEl.classList.add('method');
                epMethodEl.textContent = methodName.toUpperCase();
                epContainerEl.appendChild(epMethodEl);

                const epUrlEl = document.createElement('div');
                epUrlEl.classList.add('url');
                epUrlEl.textContent = pathName;
                epContainerEl.appendChild(epUrlEl);

                // Create extended explanation box
                const explEl = document.createElement('div');
                explEl.classList.add('explanation');
                explEl.innerHTML = `
                    <div class="header">Description</div>
                    <p class="desc">${(routeMeta.description != null) ? routeMeta.description.join('\n') : "(no description)"}</p>
                `;

                // Set endpoint container event
                epContainerEl.addEventListener('click', (e) => {
                    explEl.style.display = (explEl.style.display == "block") ? "" : "block";
                });

                // Check for parameters
                if(methodObj['parameters'] != null) {
                    explEl.appendChild(Dom.createHeader("Parameters"));

                    const paramsContainerEl = document.createElement('div');
                    paramsContainerEl.classList.add('params', 'field');

                    // Add parameters
                    for(let param of methodObj['parameters']) {
                        const paramContainer = document.createElement('div');
                        paramContainer.classList.add('parameter');

                        const typeEl = document.createElement('span');
                        typeEl.classList.add('type');
                        typeEl.textContent = param['in'];

                        const schemaEl = document.createElement('span');
                        schemaEl.classList.add('schema');
                        schemaEl.textContent = param.schema.type;

                        const nameEl = document.createElement('span');
                        nameEl.classList.add('name');
                        nameEl.textContent = (param['in'] == "path") ? `{${param['name']}}` : param['name'];

                        // Append finished elements
                        paramContainer.appendChild(typeEl);
                        paramContainer.appendChild(schemaEl);
                        paramContainer.appendChild(nameEl);

                        paramsContainerEl.appendChild(paramContainer);
                    }

                    explEl.appendChild(paramsContainerEl);
                }

                // Check for request body
                if(methodObj.requestBody != null) {
                    const reqBody = getRequestBody(methodName, pathName);

                    if(reqBody != null) {
                        explEl.appendChild(Dom.createHeader("Sample Request Body"));

                        const reqBodyEl = document.createElement('div');
                        reqBodyEl.classList.add('code-block');
                        reqBodyEl.textContent = reqBody;
                        explEl.appendChild(reqBodyEl);
                    }
                }

                // Check for response codes
                if(methodObj.responses != null) {
                    const respondCodeKeys = Object.keys(methodObj.responses);

                    explEl.appendChild(Dom.createHeader("Response Codes"));

                    // Create table for response codes
                    const rspCodesTbl = document.createElement('table');
                    rspCodesTbl.classList.add('response-codes');

                    // Add headers
                    const headerTr = document.createElement('tr');
                    headerTr.appendChild(Dom.createTableCol("Code", 100));
                    headerTr.appendChild(Dom.createTableCol("Expected Response"));
                    rspCodesTbl.appendChild(headerTr);

                    // Iterate response code keys
                    for(let code of respondCodeKeys) {
                        const tr = document.createElement('tr');

                        const rspCodeTd = document.createElement('td');
                        rspCodeTd.textContent = `${code}: ${methodObj.responses[code].description}`;
                        tr.appendChild(rspCodeTd);

                        const rspCodeSample = document.createElement('td');

                        // Check for response body
                        const rspBody = getResponseBody(parseInt(code), methodName, pathName);

                        if(rspBody != null) {
                            const rspBodyMeta = getRouteMeta(methodName, pathName).responseBodies[code];

                            if(rspBodyMeta == null)
                                continue;

                            const descEl = document.createElement('i');
                            descEl.textContent = (rspBodyMeta.description == null) ? "(no description)" : rspBodyMeta.description;
                            rspCodeSample.appendChild(descEl);

                            // Create code block
                            const codeBlock = document.createElement('div');
                            codeBlock.classList.add("code-block");
                            codeBlock.textContent = rspBody;
                            rspCodeSample.appendChild(codeBlock);
                        }
                        else
                            rspCodeSample.textContent = `(empty)`;

                        tr.appendChild(rspCodeSample);

                        rspCodesTbl.appendChild(tr);
                    }

                    explEl.appendChild(rspCodesTbl);
                }

                // Add constructed containers in order
                apiBox.appendChild(epContainerEl);
                apiBox.appendChild(explEl);

                // Add API box to document
                apiViewCt.appendChild(apiBox);
            }
        }
    }
})();