# EmotionAnalytics
A desktop application for emotional analytics using Azure Cognitive services. This project is built using .net and C#. 
Used EmguCV for camera operations and face detection. If face is detected, this stream is passed to face service API to get facial expressions.
Application takes several images of a person after specific interval and get facial expression score for them. Facial expression score for a person is average of all score of all images of him.
Detailed report is generated for all persons and summary is also generated in report like total visited, happy count, unhappy count, extremely happy count etc.
