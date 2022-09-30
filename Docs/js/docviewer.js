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

        const reqBody = route.requestBody;

        if(!reqBody)
            return null;
        
        switch(reqBody.type) {
            default:
                throw new Error("Unknown type");
            case "string":
                return reqBody.data;
            case "json":
                return JSON.stringify(reqBody.data, null, 4);
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

                    const rspCodesContainerEl = document.createElement('div');
                    rspCodesContainerEl.classList.add('params');

                    for(let code of respondCodeKeys) {
                        const rspCodeEl = document.createElement('div');
                        rspCodeEl.textContent = `${code}: ${methodObj.responses[code].description}`;
                        rspCodesContainerEl.appendChild(rspCodeEl);
                    }

                    explEl.appendChild(rspCodesContainerEl);
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