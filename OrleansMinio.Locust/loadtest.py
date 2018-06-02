from locust import HttpLocust, TaskSet, task

class MyTaskSet(TaskSet):
    @task
    def saveGrain(self):
        self.client.post("/account/balance?b=20")

class MyLocust(HttpLocust):
    task_set = MyTaskSet
    min_wait = 1000
    max_wait = 1000
    host = "http://localhost:5000"