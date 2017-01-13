app.service('monitorSvc', function ($, $rootScope) {
    var initialize = function () {
        // fetch connection object and create proxy
        var connection = $.hubConnection();
        this.proxy = connection.createHubProxy('monitors');

        // start connection
        connection.start();

        this.proxy.on('LoadBalance', function (data) {
            $rootScope.$emit("LoadBalance", data);
        });

        this.proxy.on('TransformFileToFileOrderEntity', function (data) {
            $rootScope.$emit("TransformFileToFileOrderEntity", data);
        });
    };

    return {
        initialize: initialize,
    };
});