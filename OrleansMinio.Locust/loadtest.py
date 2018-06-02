from locust import HttpLocust, TaskSet, task

class MyTaskSet(TaskSet):
    @task
    def saveGrain(self):
        self.client.post("/account/balance?b=20")

class LoadTest(HttpLocust):
    task_set = MyTaskSet
    host = "http://localhost:5000"