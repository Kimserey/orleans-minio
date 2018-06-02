from locust import HttpLocust, TaskSet, task

class MyTaskSet(TaskSet):
    @task
    def saveGrain(self):
        print("executing my_task")
        self.client.post("/account/balance?b=20")

class MyLocust(HttpLocust):
    task_set = MyTaskSet
    min_wait = 5000
    max_wait = 15000
    host = "http://localhost:5000"