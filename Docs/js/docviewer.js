(function() {
    /**
     * Entry point.
     */
    window.getDocs = async function() {
        let endpoints;

        try {
            let epReq = await fetch('data/endpoints.json');
            endpoints = await epReq.json();
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

                apiBox.appendChild(epContainerEl);

                // Add API box to document
                apiViewCt.appendChild(apiBox);
            }
        }
    }
})();