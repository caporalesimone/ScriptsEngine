class Script:
    def Run(self):
        print("Script is running")
        import time
        for i in range(5):
            if self._token.isCancellationRequested:
                print("Script was cancelled")
                return
            print(f"Step {i+1}")
            time.sleep(1)
        print("Script finished successfully")

    def Stop(self):
        self._token.cancel()

def GetScriptInstance(token):
    script = Script()
    script._token = token
    return script
