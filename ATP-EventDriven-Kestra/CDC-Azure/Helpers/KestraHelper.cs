using CDC_Azure.Helpers;
using CDC_Azure.Models;

public static class KestraHelper
{
    public static async Task<string> TriggerFlowTBiGSys(Dictionary<string, object> inputs)
    {
        var flowYaml = $@"
            id: bat_436140
            namespace: database.223.tbigsys
            tasks:
              - id: hello
                type: io.kestra.plugin.core.log.Log
                message: Hello World! 🚀
            inputs:
              - name: orderId
                type: STRING
              - name: status
                type: STRING
              - name: before
                type: JSON
              - name: after
                type: JSON
              - name: source
                type: JSON
              - name: op
                type: STRING
              - name: ts_ms
                type: STRING";

        var execId = await KestraTrigger.TriggerFlowMultipartAsync(flowYaml, inputs);

        return execId;
    }
}
