(function () {
    'use strict';

    angular
        .module('Company.gazoo.examination')
        .service('managingFilesShareDataService', managingFilesShareDataService);

    managingFilesShareDataService.$inject = [];

    function managingFilesShareDataService() {
        var audio = new Blob();

        var setAudio = function (newAudio) {
            audio = newAudio;
        };

        var getAudio = function () {
            return audio;
        };

        return {
            setAudio: setAudio,
            getAudio: getAudio
        };
    }
})();